# whirlwind-unity-plugin
Unity Plugin for Whirlwind Vortx<br/>
UNITY VERSION USED: Unity Pro 5.4.0f3 (64 bit)<br/>
ARDUINO VERSION USED: Arduino 1.6.7<br/>

<h1>WhirlwindController</h1>
>class in UnityEngine / Inherits from: MonoBehaviour

<h2>Description</h2>
The Whirlwind Controller provides access variables and functions for the Vortx haptic system.<br>

It is used to check hardware status and trigger actions for air effects such as airflow speed, temperature levels, and burst events.<br>

You may need to install the standard <a href="https://www.arduino.cc/en/Main/Software">Arduino</a> software for the Vortx device to be recognized. If you do not want to install the entire IDE, select the '/driver' folder during the Update Driver Software process in the Device Manager (Windows).

If the serial port cannot be found, check the Optimization settings under File->Build Settings, click 'Player Settings', and search under 'Other settings', choose .NET 2.0 for 'API Compatibility Level'.

The controller uses a modified <a href="https://www.assetstore.unity3d.com/en/#!/content/6804">Uniduino</a> library, which is included in the Uniduino directory of the <a href="https://bitbucket.org/chenny408/whirlwind-ui">whirlwind-ui project</a> (Vortx Utility).  You'll need to drop the folder into the Asset folder of your Unity project to begin integration.<br>

<h2>Best Practices</h2>

1. Do not leave the heater on for more than 40 minutes (to avoid overheating)
2. For maximum burst effect, turn off fan *1 second* before triggering burst
   Also, wait at least *0.5 seconds* before turning on fan after burst event
3. If possible, use multiple bursts (count > 1) for the lowest latency between bursts (120ms)

<h2>Variables</h2>
**arduino**:  Returns the Arduino instance associated with the Vortx device

<h2>Public Functions</h2>

<h3>Flow - Trigger airflow for specified duration</h3>
>public void Flow(int flevel, float duration = 0);<br>

__Parameters__<br>
flevel: airflow level [0-255]<br>
duration (optional): in seconds, default = 0 for continuous<br>

<h3>HeatedFlow - Trigger heated airflow for specified duration</h3>
>public void HeatedFlow(int hlevel, float heatCtrl, int flevel, float duration = 0);<br>

__Parameters__<br>
hlevel: heat level [0-2], <i>0 is off</i><br>
heatCtrl: heat power [0-1]<br>
flevel: airflow level [0-255]<br>
duration (optional): in seconds, default = 0 for continuous<br>

<h3>PreHeatWithFlow - Enable pre-heating (heater on with damper closed) with airflow for specified duration</h3>
>public void PreHeatWithFlow(int hlevel, int flevel, float duration = 0);<br>

__Parameters__<br>
hlevel: heat level [0-2], <i>0 is off</i><br>
flevel: airflow level [0-255]<br>
duration (optional): in seconds, default = 0 for continuous<br>

<h3>ExplosionBurst - Trigger burst events</h3>
>public void ExplosionBurst(int count, int flevel, int hlevel, float heatCtrl, float duration = 0);<br>

__Parameters__<br>
count: number of bursts [1-9]<br>
flevel: airflow level [0-255]<br>
hlevel: heat level [0-2], <i>0 is off</i><br>
heatCtrl: heat power [0-1]<br>
duration (optional): in seconds, default = 0 for continuous<br>

<h3>IsWhirlWindSystemReady - Return true if a Vortx device is connected</h3>
>public bool IsWhirlWindSystemReady();<br>

<h3>GetCOMPortName - Get the COM port where the Vortx device is connected, i.e. COM5</h3>
>public string GetCOMPortName();<br>

<h3>StopAllEffects - Turn off all device effects</h3>
>public void StopAllEffects();<br>

<h3>For Direct Control of Vortx</h3>

<h3>SetHeaters - Turn off/on heaters</h3>
>public void setHeaters(int hlevel);<br>

__Parameters__<br>
hlevel: 0 (off), 1 (1 heater on), 2 (2 heaters on - future)<br>

<h3>FanMotorWrite - Set fan speed</h3>
>public void FanMotorWrite(int flowValue);<br>

__Parameters__<br>
flowValue: set fan speed [0-255]<br>

<h3>DamperServoWrite - Set temperature level</h3>
>public void DamperServoWrite(float damperValue);<br>

__Parameters__<br>
damperValue: set damper value [0-1] based on temperature level, 0 for no heat, and 1 for full heat<br>

<h2>Firmware</h2>
If you need to update the firmware for whatever reason, you can use the precompiled binary hex files.  The with_bootloader version includes the bootloader code, which allows for serial uploads via USB.

The easiest way to perform the upload is using <a href="http://xloader.russemotto.com/">XLoader</a> for Windows.
