# whirlwind-unity-plugin
Unity Plugin for Whirlwind Vortx<br/>
UNITY VERSION USED: Unity Pro 5.3.1p4 (64 bit)<br/>
ARDUINO VERSION USED: Arduino 1.6.7<br/>

<h1>WhirlWindController</h1>
<p style="color: #99a0a7;">class in UnityEngine / Inherits from: MonoBehaviour</p>

<h2>Description</h2>
The Whirlwind Controller provides access variable and functions for the Vortx haptic system.<br>
It is used to check hardware status and trigger actions for air effects such as airflow speed, temperature levels, and burst events.<br>

<h2>Variables</h2>
<u>arduino</u>   Returns the Arduino instance associated with the Vortx device

<h2>Public Functions</h2>

Flow - Trigger airflow for duration<br>
public void Flow(int flevel, float duration = 0);<br>

<h3>Parameters</h3>
flevel: airflow level [0-255]<br>
duration (optional): in seconds, default = 0 for indefinite<br>

HeatedFlow - Trigger heated airflow for duration<br>
public void HeatedFlow(int hlevel, float heatCtrl, int flevel, float duration = 0);<br>

<h3>Parameters</h3>
hlevel: heat level [0-2]<br>
heatCtrl: heat power [0-1]<br>
flevel: airflow level [0-255]<br>
duration (optional): in seconds, default = 0 for indefinite<br>

PreHeatWithFlow - Enable pre-heating (heater on with damper closed) with airflow for duration.<br>
public void PreHeatWithFlow(int hlevel, int flevel, float duration = 0)<br>

<h3>Parameters</h3>
hlevel: heat level [0-2]<br>
flevel: airflow level [0-255]<br>
duration (optional): in seconds, default = 0 for indefinite<br>

ExplosionBurst - Trigger burst events<br>
public void ExplosionBurst(int count, int flevel, int hlevel, float heatCtrl, float duration = 0)<br>

<h3>Parameters</h3>
count: number of bursts [1-9]<br>
flevel: airflow level [0-255]<br>
hlevel: heat level [0-2]<br>
heatCtrl: heat power [0-1]<br>
duration (optional): in seconds, default = 0 for indefinite<br>

IsWhirlWindSystemReady - Check if the device is ready to send/receive commands<br>
public bool IsWhirlWindSystemReady();<br>

GetCOMPortName - Get the COM port name where the device is connected, i.e. COM5<br>
public string GetCOMPortName ();<br>

StopAllEffects - Turn off all device effects<br>
public void StopAllEffects();<br>

