//#define IS_IRON_MAN
//#define IS_THOR
#define IS_HULK

using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Uniduino;
using Uniduino.Helpers;
using System.IO.Ports;
using System;

enum HeatLevel 
{
	heat0=0,//no heat
	heat1=1,//1 heating coil
	heat2=2,//2 heating coils (future)
}

enum FanLevel
{
	fan0=0, //fan off
	fan1=95, //moving air
	fan2=130, //strong breeze
	fan3=170, //creates wind flow
	fan4=200, //used for strong wind flow
	fan5=245 //used for powerful explosions and strong gusts of wind
}

enum Pins
{
	FanPin = 11, //fan motor speed control [0-255]
	damperPin = 5, //damper setting using a Servo to control hot or cold flow
	limSwitchPin = 12, //limit switch for drive train. Starts with HIGH, released to LOW, then HIGH again after full burst (complete gear rotation)
	burstPin = 3, //burst motor speed pin, 0-255 corresponds to 100RPM to 10,000RPM; not used to control on/off
	heatPin1 = 7, //control on/off of heaters. active high.
	heatPin2 = 8, //control on/off of heaters. active high. (future)
	burstControlPin = 4, // initiaite burst
	redPin = 9, //red LED pin
	greenPin = 6, //green LED pin
	bluePin = 10, //blue LED pin
	fanBrakePin = 14
}

#if IS_HULK
enum DefaultParameters //HULK
{
	defaultHeat=0, //normal off state
	defaultFan=90, // normal wind state
	damperAmbient = 125, // damper position allows ambient
	damperHeat = 55 // damper position allows max heat
}
#endif

#if IS_IRON_MAN
enum DefaultParameters //IRON_MAN
{
defaultHeat=0, //normal off state
defaultFan=90, // normal wind state
damperAmbient = 0, // damper position allows ambient
damperHeat = 52 // damper position allows max heat
}
#endif

#if IS_THOR
enum DefaultParameters //THOR
{
defaultHeat=0, //normal off state
defaultFan=90, // normal wind setting
damperAmbient = 170, // damper position allows ambient
damperHeat = 112 // damper position allows max heat
}
#endif

public class WhirlWindController : MonoBehaviour {
	public Arduino arduino;

	int CurrentHeatLevel = (int)HeatLevel.heat0; //used to keep track of the current heating level
	float CurrentHeatPower = (int)HeatLevel.heat0; // heat power as % [0-1]
	int CurrentFlowLevel = (int)FanLevel.fan0; //used to keep track of the current fan speed
	int CurrentCoil = 1; //used to keep track of recent coil. Alternate to reduce overheat. (future)

	int Red = 0;
	int Green = 0;
	int Blue = 0;

	bool DeviceReady = false;
	bool HitBurst = false;
	bool InitializedHeatersComplete = false;

	void Awake()
	{
		Application.targetFrameRate = 300; //set a high frame rate on Windows
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
		moveDamper ((int)HeatLevel.heat0); //initialize position

		//HEAT PINS
		arduino.pinMode((int)Pins.heatPin1, PinMode.OUTPUT);
		arduino.pinMode((int)Pins.heatPin2, PinMode.OUTPUT);

		arduino.pinMode((int)Pins.burstControlPin, PinMode.OUTPUT);
		arduino.pinMode((int)Pins.fanBrakePin, PinMode.OUTPUT);
		
		//COLOR LEDS
		arduino.pinMode((int)Pins.redPin, PinMode.PWM);
		arduino.pinMode((int)Pins.greenPin, PinMode.PWM);
		arduino.pinMode((int)Pins.bluePin, PinMode.PWM);
	}

	// Initialize arduino pins and set default values for fan and heating
	IEnumerator Initialize()
	{
		arduino.digitalWrite ((int)Pins.burstControlPin, Arduino.LOW);
		arduino.digitalWrite ((int)Pins.fanBrakePin, Arduino.LOW);
		
		yield return new WaitForSeconds (16 / 1000);
		flowNow((int)FanLevel.fan0, (int)HeatLevel.heat0, 0, 0); //turn everything off
	}

	// Update is called once per frame
	void Update () 
	{
		if (arduino.IsWhirlWindSystemReady ()) 
		{
			if (!InitializedHeatersComplete) 
			{
				InitializeHeaters (); // make sure heaters are off
				//SetLEDRingEXit(255,0,255);
			}

			//UpdateLEDRing (); // update the LEDs for different features
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
		SetHeaters ((int)HeatLevel.heat0);
		DamperServoWrite (0); // start damper at ambient side
		InitializedHeatersComplete = true;
	}

	// transition the green light when device turns on
	IEnumerator SetLEDRingStart(int r, int g, int b)
	{
		int intensity = 0;

		yield return new WaitForSeconds (0);
		arduino.analogWrite((int)Pins.redPin, r);
		arduino.analogWrite((int)Pins.bluePin, b);

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

	// sets the fan/heat level and move damper
	void flowNow(int flevel, int hlevel, float heatCtrl, float duration = 0) 
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

		arduino.analogWrite((int)Pins.FanPin, (int)flevel); //set fan level
		SetHeaters((int)hlevel); //set heat level
		moveDamper(heatCtrl); //move damper

		if (duration != 0) {
			StartCoroutine(triggerDuration ((int)duration)); //keep the environmental condition on for the specified time in seconds
		}
	}

	IEnumerator triggerDuration(int sec)
	{
		yield return new WaitForSeconds (sec);
		StopAllEffects();
	}

	// Set Heating Level 0, 1 or 2, active high (for video editor)
	public void SetHeaters(int hlevel) 
	{
		if (hlevel > 0) {
			CurrentHeatLevel = 2; //use both heaters (default)
		} else {
			CurrentHeatLevel = 0;
		}

		if (CurrentHeatLevel == 1) 
		{     
			if (CurrentCoil == 1) 
			{
				arduino.digitalWrite((int)Pins.heatPin1, Arduino.LOW);
				arduino.digitalWrite((int)Pins.heatPin2, Arduino.HIGH);
				CurrentCoil = 2;
			}
			else if (CurrentCoil == 2) 
			{
				arduino.digitalWrite((int)Pins.heatPin1, Arduino.HIGH);
				arduino.digitalWrite((int)Pins.heatPin2, Arduino.LOW);
				CurrentCoil = 1;
			}
		}
		else if (CurrentHeatLevel == 2) 
		{
			arduino.digitalWrite((int)Pins.heatPin1, Arduino.HIGH);
			arduino.digitalWrite((int)Pins.heatPin2, Arduino.HIGH);
		}
		else //heat off
		{
			arduino.digitalWrite((int)Pins.heatPin1, Arduino.LOW);
			arduino.digitalWrite((int)Pins.heatPin2, Arduino.LOW);
		}
	}

	// move damper to the hot or ambient position based on heating level (%).
	void moveDamper(float hlevel) 
	{ 
		if (hlevel == (int)HeatLevel.heat0) 
		{
			arduino.analogWrite ((int)Pins.damperPin, (int)DefaultParameters.damperAmbient); //ambient
		} 
		else if (hlevel == (int)HeatLevel.heat1) 
		{
			arduino.analogWrite((int)Pins.damperPin, (int)DefaultParameters.damperHeat); //max heat
		}
		else 
		{
			int damperPos = calculate_servo_pos (hlevel); //calculate damper position based on heat level
			DamperServoWrite ((int)damperPos);
		}
	}

	/* public functions */

	// Sets the fan level (only) to produce flows with no heating
	public void Flow(int flevel, float duration = 0)
	{
		//Debug.Log ("Flow: flow = " + flevel + ", duration = " + duration);
		flowNow (flevel, (int)HeatLevel.heat0, 0, duration);
	}

	// Sets the fan level and heat level to produce heated flows
	public void HeatedFlow(int hlevel, float heatCtrl, int flevel, float duration = 0)
	{
		//Debug.Log ("HeatedFlow: flow = " + flevel + " , heat = " + hlevel + ", power = " + heatCtrl + ", duration = " + duration);
		flowNow (flevel, hlevel, heatCtrl, duration);
	}

	//Pre-Heat with Fan
	public void PreHeatWithFlow(int hlevel, int flevel, float duration = 0)
	{
		//Debug.Log ("PreHeatWithFlow: flow = " + flevel + " , heat = " + hlevel + ", duration = " + duration);
		arduino.analogWrite((int)Pins.FanPin, (int)flevel);
		SetHeaters(hlevel);
		moveDamper((int)HeatLevel.heat0);

		StartCoroutine (until (duration));
	}

	// trigger single or multiple bursts at the specified heat level. Duration is the time (in seconds) for which the fan/heat remains on after burst(s)
	public void ExplosionBurst(int count, int flevel, int hlevel, float heatCtrl, float duration = 0)
	{
		if (count == 0)
			return;

		count = Mathf.Clamp(count, 1, 9); //limit to 9 bursts

		HitBurst = true;
		//Debug.Log ("ExplosionBurst: burst count = " + count + ", flow = " + flevel + " , heat = " + hlevel + ", power = " + heatCtrl + ", duration = " + duration);
		arduino.WriteBurstCountToArduino(count.ToString());

		CurrentFlowLevel = flevel;
		CurrentHeatLevel = hlevel;
		CurrentHeatPower = heatCtrl;

		StartCoroutine (until (duration));
	}


	// Check if the device is ready to send/receive commands
	public bool IsWhirlWindSystemReady()
	{
		return arduino.IsWhirlWindSystemReady ();
	}

	// Get the COM port name where the device is connected
	public string GetCOMPortName ()
	{
		return arduino.PortName;
	}

	// Turn off all device effects
	public void StopAllEffects()
	{
		Debug.Log ("StopAllEffects called..");

		arduino.analogWrite ((int)Pins.burstPin, 0);
		flowNow((int)FanLevel.fan0, (int)HeatLevel.heat0, 0, 0);
		DamperServoWrite (0);
		SetLEDRingEXit (0, 0, 0);
	}

	/* end public functions */

	IEnumerator until(float durationSeconds) 
	{
		// if the duration value is 0, will run continuously at current heat/fan
		if (durationSeconds == 0) 
		{ 
			yield return new WaitForSeconds (0f);
		}
		else 
		{
			yield return new WaitForSeconds (durationSeconds);

			//Debug.Log("Resetting device..");
			CurrentFlowLevel = (int)FanLevel.fan0;
			CurrentHeatLevel = (int)HeatLevel.heat0;
			CurrentHeatPower = (int)HeatLevel.heat0;
			flowNow(CurrentFlowLevel, CurrentHeatLevel, CurrentHeatPower, 0); // returns to the set default levels of the fan and heaters
		}
	}

	// Sets the fan to the specified speed (for video player)
	public void FanMotorWrite(int flowValue)
	{
		if (flowValue> 0) 
		{
			Red = 0;
			Green = 0;
			Blue = 255;
		} 
		else 
		{
			Red = 0;
			Green = 255;
			Blue = 0;
		}

		arduino.analogWrite((int)Pins.FanPin,flowValue);
	}

	// calculate damper position based on heat power
	int calculate_servo_pos(float damperPos) 
	{
		float servo_offset = (DefaultParameters.damperHeat - DefaultParameters.damperAmbient) * damperPos;
		int servo_pos = (int)DefaultParameters.damperAmbient + (int)servo_offset; //offset could be negative
		//Debug.Log ("servo: level = " + damperPos + ", offset = " + servo_offset + ", pos = " + servo_pos);

		return (int) servo_pos;
	}

	// Sets the damper to the specified value based on temperature level
	public void DamperServoWrite(float damperValue)
	{
		arduino.analogWrite ((int)Pins.damperPin, calculate_servo_pos(damperValue));
	}
}
