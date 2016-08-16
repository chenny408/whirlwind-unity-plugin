# whirlwind-unity-plugin
Unity Plugin for Whirlwind Vortx<br/>
UNITY VERSION USED: Unity Pro 5.4.0f3 (64 bit)<br/>
ARDUINO VERSION USED: Arduino 1.6.7<br/>

<h1>WhirlwindController</h1>
>class in UnityEngine / Inherits from: MonoBehaviour

<h2>Description</h2>
The Whirlwind Controller provides access variable and functions for the Vortx haptic system.<br>

It is used to check hardware status and trigger actions for air effects such as airflow speed, temperature levels, and burst events.<br>

<h2>Variables</h2>
**arduino**:  Returns the Arduino instance associated with the Vortx device

<h2>Public Functions</h2>

<h3>Flow - Trigger airflow for specified duration</h3>
>public void Flow(int flevel, float duration = 0);<br>

__Parameters__<br>
flevel: airflow level [0-255]<br>
duration (optional): in seconds, default = 0 for indefinite<br>

<h3>HeatedFlow - Trigger heated airflow for specified duration</h3>
>public void HeatedFlow(int hlevel, float heatCtrl, int flevel, float duration = 0);<br>

__Parameters__<br>
hlevel: heat level [0-2]<br>
heatCtrl: heat power [0-1]<br>
flevel: airflow level [0-255]<br>
duration (optional): in seconds, default = 0 for indefinite<br>

<h3>PreHeatWithFlow - Enable pre-heating (heater on with damper closed) with airflow for specified duration</h3>
>public void PreHeatWithFlow(int hlevel, int flevel, float duration = 0)<br>

__Parameters__<br>
hlevel: heat level [0-2]<br>
flevel: airflow level [0-255]<br>
duration (optional): in seconds, default = 0 for indefinite<br>

<h3>ExplosionBurst - Trigger burst events</h3>
>public void ExplosionBurst(int count, int flevel, int hlevel, float heatCtrl, float duration = 0);<br>

__Parameters__<br>
count: number of bursts [1-9]<br>
flevel: airflow level [0-255]<br>
hlevel: heat level [0-2]<br>
heatCtrl: heat power [0-1]<br>
duration (optional): in seconds, default = 0 for indefinite<br>

<h3>IsWhirlWindSystemReady - Check if the device is ready to send/receive commands</h3>
>public bool IsWhirlWindSystemReady();<br>

<h3>GetCOMPortName - Get the COM port where the Vortx device is connected, i.e. COM5</h3>
>public string GetCOMPortName();<br>

<h3>StopAllEffects - Turn off all device effects</h3>
>public void StopAllEffects();<br>

<h3>For Direct Control of Vortx</h3>

<h3>SetHeaters - Turn off/on heaters</h3>
>public void setHeaters(int hlevel);<br>

__Parameters__<br>
hlevel: 0 (off), 1 (1 heater), 2 (2 heaters - future)<br>

<h3>FanMotorWrite - Set fan speed</h3>
>public void FanMotorWrite(int flowValue);<br>

__Parameters__<br>
flowValue: set fan speed [0-255]<br>

<h3>DamperServoWrite - Set temperature level</h3>
>public void DamperServoWrite(float damperValue);<br>

__Parameters__<br>
damperValue: set damper value [0-1] based on temperature level, 0 for no heat, and 1 for full heat<br>
