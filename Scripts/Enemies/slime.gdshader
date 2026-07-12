shader_type canvas_item;

uniform bool flash;
uniform vec3 col;

void fragment() {
	vec4 tex = texture(TEXTURE, UV);
	if (flash){
		tex.rgb = col;
	}
	COLOR = tex;
}
