using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Uniduino;
using Uniduino.Helpers;

enum HeatLevel 
{
	heat0=0,//no heat
	heat1=1,//1 heating coil
	heat2=2,//2 heating coils
}

//THOR
enum FanLevel
{
	fan0=0, //fan off
	fan1=95, //moving air
	fan2 =130, //strong breeze
	fan3 =170, //creates wind flow
	fan4 =200, //used for large flow of wind
	fan5 =245 //used for powerful explosions and strong gusts of wind
}

enum Pins
{
	FanPin = 11,//fan motor speed control 0-255
	damperPin = 5,//damper, Servo that controlls hot or cold flow, if 50 will allow ambient flow, if 110 will flow to heat
	limSwitchPin = 12,// 12new limit switch in drive train. Starts pressed HIGH, released to LOW, HIGH again once full burst gear turn
	burstPin = 3,//burst motor speed pin 0-255 corresponds to 100RPM to 10,000RPM; not used to control on/off
	heatPin1 = 7,//control on/off of heaters. If pin is low, will turn heater on
	heatPin2 = 8,//control on/off of heaters. If pin is low, will turn heater on
	burstControlPin = 4, // sets off burst
	redPin = 9, //red LED pin
	greenPin = 6, //green LED pin
	bluePin = 10, //blue LED pin
}

enum DefaultParameters
{
	defaultHeat=0, //normal off state
	defaultFan=90, // normal wind and heat amounts
	damperAmbient = 170, // damper position allows ambient
	damperHeat = 113 // damper position allows heat
}



public class WhirlWindController : MonoBehaviour {



	public Arduino arduino;

	int CurrentHeatLevel = (int)HeatLevel.heat0; // used to keep track of the current heating level
	int CurrentFlowLevel = (int)FanLevel.fan0; //used to keep track of the current fan speed
	int CurrentCoil = 1;// used to keep track of which coils have been recently used. Alternate to reduce overheat
	float currentBurstDuration = 0f;
	bool InitializedHeatersComplete = false;


	int Red = 0;
	int Green = 0;
	int Blue = 0;
	bool DeviceReady = false;
	bool HitBurst = false;

	void Awake()
	{
		Application.targetFrameRate = 300; //set a high frame rate on windows
	}

	// Use this for initialization
	void Start () 
	{
		arduino = Arduino.global;
		arduino.Log = (s) => Debug.Log("Arduino: " +s);

		arduino.Setup(ConfigurePins);
		StartCoroutine(Initialize ());

	}


	// Configuration of Pins on the Arduino
	void ConfigurePins () 
	{

		arduino.pinMode((int)Pins.burstPin, PinMode.PWM);
		arduino.pinMode((int)Pins.FanPin, PinMode.PWM);

		//DAMPER
		arduino.pinMode((int)Pins.damperPin, PinMode.SERVO);
		moveDamper (0);

		//HEAT PINS
		arduino.pinMode((int)Pins.heatPin1, PinMode.OUTPUT); //set pins as outputs or inputs
		arduino.pinMode((int)Pins.heatPin2, PinMode.OUTPUT);

		arduino.pinMode((int)Pins.burstControlPin, PinMode.OUTPUT);

		//COLOR LED'S
		arduino.pinMode((int)Pins.redPin, PinMode.PWM);
		arduino.pinMode((int)Pins.greenPin, PinMode.PWM);
		arduino.pinMode((int)Pins.bluePin, PinMode.PWM);

	}

	// Initialize arduino pins and set default values for fan and heating
	IEnumerator Initialize()
	{

		arduino.digitalWrite ((int)Pins.burstControlPin, Arduino.LOW);

		yield return new WaitForSeconds (16 / 1000);
		flowNow((int)FanLevel.fan0, (int)HeatLevel.heat0,0); //turn everything off

	}


	// Update is called once per frame
	void Update () 
	{

		if (arduino.IsWhirlWindSystemReady ()) 
		{
			if (!InitializedHeatersComplete) 
			{
				InitializeHeaters (); // make sure heaters are off
				//StartCoroutine(SetLEDRingStart(0, 255, 0)); // green light was not working 
				SetLEDRingEXit(255,0,255);
			}
				
			UpdateLEDRing (); // update the LED's for different features
		}
	}
		
	// updates the lED colors based on features and functions
	void UpdateLEDRing()
	{
		if (DeviceReady) 
		{
			if (!HitBurst) 
			{
				arduino.analogWrite((int)Pins.redPin, Red);
				arduino.analogWrite((int)Pins.greenPin, Green);
				arduino.analogWrite((int)Pins.bluePin, Blue);
			}
		}


	}

	// Heaters turned off
	void InitializeHeaters()
	{
		setHeaters (0);

		//Debug.Log ("heaters initialized");
		InitializedHeatersComplete = true;

		DamperServoWrite ((int)DefaultParameters.damperAmbient); // start damper at ambient side
	}

	// transition the green light when device turns on
	IEnumerator SetLEDRingStart(int r, int g, int b)
	{
		yield return new WaitForSeconds (0);
		arduino.analogWrite((int)Pins.redPin, r);
		arduino.analogWrite((int)Pins.bluePin, b);

		int intensity = 0;

		while (intensity <= 255) 
		{
			arduino.analogWrite((int)Pins.greenPin, intensity);
			yield return new WaitForSeconds (0.02f);
			intensity += 10;
		}

		DeviceReady = true;
	}

	// set the LED rings 
	void SetLEDRingEXit(int r, int g, int b)
	{
		arduino.analogWrite((int)Pins.greenPin, g);
		arduino.analogWrite((int)Pins.redPin, r);
		arduino.analogWrite((int)Pins.bluePin, b);

	}

	// sets the fan/heat level andf moves damper to the required side
	void flowNow( int flevel, int hlevel,float duration) 
	{

		if (hlevel == 0 && flevel == 0) 
		{
			Red = 0;
			Green = 255;
			Blue = 0;
		} 
		else if (hlevel != 0) 
		{
			Red = 255;
			Green = 0;
			Blue = 0;
		} 
		else if (hlevel == 0 && flevel != 0) 
		{
			Red = 0;
			Green = 0;
			Blue = 255;
		}

		arduino.analogWrite((int)Pins.FanPin, (int)flevel);
		setHeaters((int)hlevel);
		moveDamper(hlevel);

		StartCoroutine (until (duration)); // used to keep the environmental condition on for the specified time in seconds
	}


	// Set Heating Level 0, 1 or 2, low means ON, High means OFF
	public void setHeaters(int hlevel) 
	{
		CurrentHeatLevel = hlevel;

		if (CurrentHeatLevel == 1) 
		{//alternates coils used

			if (CurrentCoil == 1) 
			{
				arduino.digitalWrite((int)Pins.heatPin1, Arduino.LOW);//pin 7
				arduino.digitalWrite((int)Pins.heatPin2, Arduino.LOW);//pin 8
				//CurrentCoil++;
			}
			else if (CurrentCoil == 2) 
			{
				arduino.digitalWrite((int)Pins.heatPin1, Arduino.LOW);
				arduino.digitalWrite((int)Pins.heatPin2, Arduino.LOW);
				CurrentCoil = 1;
			}
		}
		else if (CurrentHeatLevel == 2) 
		{

			arduino.digitalWrite((int)Pins.heatPin1, Arduino.LOW);
			arduino.digitalWrite((int)Pins.heatPin2, Arduino.LOW);

		}

		else 
		{

			arduino.digitalWrite((int)Pins.heatPin1, Arduino.HIGH);
			arduino.digitalWrite((int)Pins.heatPin2, Arduino.HIGH);

		}

		//delay(10);
	}


	// move damper to the hot or ambient side based on heating level.
	public void moveDamper(float hlevel) 
	{ 
		if (hlevel == (int)HeatLevel.heat0) 
		{
			arduino.analogWrite((int)Pins.damperPin, (int)DefaultParameters.damperAmbient);
		}
		else 
		{
			arduino.analogWrite((int)Pins.damperPin, (int)DefaultParameters.damperHeat);
		}
	}


	// Sets the fan level (only) to produce flows with no heating
	public void Flow(int flevel,float duration)
	{
		flowNow (flevel, (int)HeatLevel.heat0,duration);
	}

	// Sets the fan level and heat level to produce heated flows
	public void HeatedFlow(int hlevel,int flevel,float duration)
	{

		flowNow (flevel, hlevel,duration);
	}
		
	//Pre-Heat with Fan
	public void PreHeatWithFlow(int hlevel,int flevel,float duration)
	{
		PreHeatflowNow (flevel, hlevel,duration);
	}


	//immediately set the fan and heat level without checking the current time
	void PreHeatflowNow( int flevel, int hlevel,float duration) 
	{
		arduino.analogWrite((int)Pins.FanPin, (int)flevel);
		setHeaters((int)hlevel);
		moveDamper(0);

		StartCoroutine (until (duration));
	}


	// Used to cause single or multiple bursts at the specified heating level. Duration means the time (in seconds) for which the fan remains on thereafter.
	public void ExplosionBurst(int count,int flevel,int hlevel,float duration)
	{

		if (count > 9)
			count = 9;
		if (count < 0)
			count = 0;

		if (count == 0)
			return;

		HitBurst = true;
		StartCoroutine (BurstTimer (count));

		currentBurstDuration = duration;

		CurrentFlowLevel = flevel;
		CurrentHeatLevel = hlevel;

	}


	// The burst is controlled by the arduino firmata code as unity update is not fast enough to catch the limit switch changes.
	IEnumerator BurstTimer(int count)
	{
		yield return new WaitForSeconds (0);

		string strnum = ((char)((count + 97)-1)).ToString();  //ASCII 1-9


		arduino.pinMode ((int)Pins.burstControlPin, PinMode.OUTPUT);
		arduino.digitalWrite ((int)Pins.burstControlPin, Arduino.HIGH);

		arduino._serialPort.Write (strnum);
		yield return new WaitForSeconds (0.02f);


		arduino.digitalWrite ((int)Pins.burstControlPin, Arduino.LOW);

		yield return new WaitForSeconds (currentBurstDuration);

		currentBurstDuration = 0;
		HitBurst = false;
	}

	// if the duration value is 0, device will run continuously at current heat/fan			
	IEnumerator until(float durationSeconds) 
	{
		//if the duration value is 0, will run continuously at current heat/fan
		if (durationSeconds == 0) 
		{ 
			yield return new WaitForSeconds (0f);
		}
		else 
		{
			yield return new WaitForSeconds (durationSeconds);
			//flowNow(DefaultParameters.defaultFan, DefaultParameters.defaultHeat);// returns to the set default levels of the fan and heaters
			CurrentFlowLevel = (int)FanLevel.fan0;
			CurrentHeatLevel = (int)HeatLevel.heat0;
			flowNow(CurrentFlowLevel, CurrentHeatLevel,0);
		}
	}

	// Check if the device is ready to send/receive commands
	public bool IsWhirlWindSystemReady()
	{
		return arduino.IsWhirlWindSystemReady ();
	}

	// gets the COM port name where the device is connected
	public string GetCOMPortName ()
	{
		return arduino.PortName;
	}

	// Sets the fan to the specified speed
	public void FanMotorWrite(int flowPWMSliderValue)
	{
		if (flowPWMSliderValue> 0) 
		{
			Red = 0;Green = 0;Blue = 255;
		} 
		else 
		{
			Red = 0;Green = 255;Blue = 0;
		}

		arduino.analogWrite((int)Pins.FanPin,flowPWMSliderValue);
	}

	// Sets the damper to the specified angle
	public void DamperServoWrite(int damperVal)
	{
		arduino.analogWrite((int)Pins.damperPin, damperVal);
	}

	// Turns off all the device effects - fan, heating, etc.
	public void StopAllEffects()
	{
		setHeaters (0);

		arduino.analogWrite ((int)Pins.burstPin, 0);

		flowNow((int)FanLevel.fan0, (int)HeatLevel.heat0,0);

		SetLEDRingEXit (0, 0, 0);
	}
}
