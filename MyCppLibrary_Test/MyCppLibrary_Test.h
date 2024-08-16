#define MYLIBRARY_API __declspec(dllexport)
#pragma once

typedef struct {
	int x;
	int y;
} Point;

extern "C" MYLIBRARY_API int Sum(Point p);
extern "C" MYLIBRARY_API int Multi(Point p);

