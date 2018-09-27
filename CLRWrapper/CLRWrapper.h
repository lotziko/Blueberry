#pragma once

#include <string>

namespace CLRWrapper {
	public ref class Clipboard abstract sealed
	{
	public:
		static void SetText(System::String^);
		static System::String^ GetText();
		static void SetImage(array<uint32_t>^ data, int32_t width, int32_t height);
		static array<uint32_t>^ GetImage();
	};
}
