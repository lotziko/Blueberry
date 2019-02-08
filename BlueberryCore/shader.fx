attribute vec2 in_Position;
attribute vec4 in_Colour;
attribute vec2 in_TextureCoord;

uniform mat4 projection; 
uniform mat4 transform;

varying vec2 v_vTexcoord;
varying vec4 v_vColour;

void main()
{
	gl_Position = projection * transform * vec4(coord, 1.0, 1.0);

	v_vColour = in_Colour;
	v_vTexcoord = in_TextureCoord;
}


uniform sampler2d baseTexture;

varying vec2 v_vTexcoord;
varying vec4 v_vColour;

void main()
{
	gl_FragColor = v_vColour * texture2D(baseTexture, v_vTexcoord);
}
