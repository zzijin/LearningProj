#include "pch.h"
#include "MyCppLibrary_Test.h"

MYLIBRARY_API int Sum(Point p)
{
	return p.x + p.y;
}

MYLIBRARY_API int Multi(Point p)
{
	return p.x * p.y;
}