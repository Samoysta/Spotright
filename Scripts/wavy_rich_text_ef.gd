@tool

extends RichTextEffect
class_name wavyEffect

var bbcode = "wavy"

func _process_custom_fx(char_fx: CharFXTransform):
	var amp = char_fx.env.get("amp" , 1.0)
	var freq = char_fx.env.get("freq", 1.0)
	var length = char_fx.env.get("length", 5.0)

	char_fx.offset.y = amp * sin(char_fx.elapsed_time * freq + char_fx.relative_index * length)
	pass
