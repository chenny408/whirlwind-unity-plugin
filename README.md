# whirlwind-unity-plugin
Unity Plugin for Whirlwind Vortx<br/>
UNITY VERSION USED: Unity Pro 5.3.1p4 (64 bit)<br/>
ARDUINO VERSION USED: Arduino 1.6.7<br/>

<h1>WhirlWindController</h1>
<p style="color: #99a0a7;">class in UnityEngine / Inherits from: MonoBehaviour</p>

<h2>Description</h2>
The Whirlwind Controller provides access variable and functions for the Vortx haptic system.<br><br>

It is used to check hardware status and trigger actions for air effects such as airflow speed, temperature levels, and burst events.<br>

<h2>Variables</h2>
arduino   Returns the Arduino instance associated with the Vortx device

<h2>Public Functions</h2>
arduino
Flow(int flevel, float duration = 0)

HeatedFlow(int hlevel, float heatCtrl, int flevel, float duration = 0)
