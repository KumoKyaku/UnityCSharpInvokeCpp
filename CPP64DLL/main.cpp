#include <iostream>

#ifdef __cplusplus    // If used by C++ code, 
extern "C"
{          // we need to export the C interface
#endif

	struct Character
	{
	public:
		float x;
		float y;
		float z;

		float vx;
		float vy;
		float vz;

		float angle;
		float vAngle;

		float pad1;
		float pad2;
		float pad3;
		float pad4;
		float pad5;
		float pad6;
		float pad7;
		float pad8;


	};


__declspec(dllexport) int AddFun(int a, int b)
{
	return (a + b);
}

__declspec(dllexport) int SubFun(int a, int b)
{
	return (a - b);
}

__declspec(dllexport) void UpdateCharacters(void* characterArray, int arraySize)
{
		int len = arraySize;
	for (int i = 0; i < len; ++i)
	{
		///8是指针大小
		Character* element = *(Character**)((long long)characterArray + i * 8);
		element->x += element->vx;
		element->y += element->vy;
		element->z += element->vz;

		element->angle += element->vAngle;
	}
	//return characterArray[0].x;
}

#ifdef __cplusplus
}
#endif