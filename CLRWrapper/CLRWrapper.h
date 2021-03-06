#pragma once

#include <string>

namespace CLRWrapper {
	public ref class Clipboard abstract sealed
	{
	public:
		static void SetText(System::String^);
		static System::String^ GetText();
		static void SetImage();
	};
}
