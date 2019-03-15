#version 430

//$define

#ifdef VERTEX

attribute vec3 POSITION0;
attribute vec4 COLOR0;

uniform mat4 TransformMatrix;
varying vec4 v_vColor;

void main()
{
	gl_Position = TransformMatrix * vec4(POSITION0, 1.0);
	v_vColor = COLOR0;
}

#endif

#ifdef FRAGMENT

varying vec4 v_vColor;

void main()
{
	gl_FragColor = v_vColor;
}

#endif