#version 430

//$define

#ifdef VERTEX

attribute vec3 POSITION0;
attribute vec4 COLOR0;
attribute vec2 TEXCOORD0;

uniform mat4 TransformMatrix;

varying vec2 v_vTexcoord;
varying vec4 v_vColor;

void main()
{
	gl_Position = TransformMatrix * vec4(POSITION0, 1.0);
	
	v_vColor = COLOR0;
	v_vTexcoord = TEXCOORD0;
}

#endif

#ifdef FRAGMENT


uniform sampler2D tex;

varying vec2 v_vTexcoord;
varying vec4 v_vColor;

void main()
{
	gl_FragColor = v_vColor * texture2D(tex, v_vTexcoord);
}
				
#endif