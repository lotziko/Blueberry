#include "stdafx.h"

#include "CLRWrapper.h"

#include <string>

#include "clip.h"


#include <msclr\marshal_cppstd.h>
using namespace msclr::interop;

void CLRWrapper::Clipboard::SetText(System::String^ text)
{
	clip::set_text(marshal_as<std::string>(text));
}
System::String^ CLRWrapper::Clipboard::GetText()
{
	std::string value;
	clip::get_text(value);
	return marshal_as<System::String^>(value);
}
void CLRWrapper::Clipboard::SetImage()
{
	uint32_t data[] = {
   0xffff0000, 0xff00ff00, 0xff0000ff,
   0x7fff0000, 0x7f00ff00, 0x7f0000ff,
	};
	clip::image_spec spec;
	spec.width = 3;
	spec.height = 2;
	spec.bits_per_pixel = 32;
	spec.bytes_per_row = spec.width * 4;
	spec.red_mask = 0xff;
	spec.green_mask = 0xff00;
	spec.blue_mask = 0xff0000;
	spec.alpha_mask = 0xff000000;
	spec.red_shift = 0;
	spec.green_shift = 8;
	spec.blue_shift = 16;
	spec.alpha_shift = 24;
	clip::image img(data, spec);
	clip::set_image(img);
}