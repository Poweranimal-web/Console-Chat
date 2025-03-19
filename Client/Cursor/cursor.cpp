#include <X11/Xlib.h>
#include <X11/Xutil.h>
#include <iostream>
extern "C"{
    void getCursorPosition(int* row, int* col) {
        Display* dpy = XOpenDisplay(NULL);
        if (dpy == NULL) {
            std::cerr << "Unable to open display" << std::endl;
            *row = -1;
            *col = -1;
            return;
        }

        Window root = DefaultRootWindow(dpy);
        Window ret_root, ret_child;
        int root_x, root_y;
        int win_x, win_y;
        unsigned int mask;

        if (XQueryPointer(dpy, root, &ret_root, &ret_child, &root_x, &root_y, &win_x, &win_y, &mask)) {
            *row = root_x;
            *col = root_y;
        } else {
            *row = -1;
            *col = -1;
        }

        XCloseDisplay(dpy);
    }
}

