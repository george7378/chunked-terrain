# chunked-terrain
>For a similar terrain system which uses quadtrees, check out [this project](https://github.com/george7378/quadtree-terrain)

Chunked Terrain is a C#/MonoGame program demonstrating a chunked procedural terrain algorithm which can be used to render very large-scale worlds.

As you move around the terrain, the world will update dynamically to ensure that geometry is only shown where it's needed. The scene is built using deterministic noise which can be modified by changing the parameters in the code. There is also a water plane which adds a bit of variety to the landscape, and the simple procedural forests add some decor to the world.

You can explore using the mouse and keyboard. When the program starts, there may be a few seconds of delay while the terrain loads in for the first time. The **C** key toggles whether the camera is attached to the mouse, and the **Space** key will allow you to walk across the landscape or fly through the air. You can use the **WASD** keys to move around.

![Sitting under a tree and reflecting](https://raw.githubusercontent.com/george7378/chunked-terrain/master/_img/1.png)
![Forests, water and sunlight](https://raw.githubusercontent.com/george7378/chunked-terrain/master/_img/2.png)
![Some calm lakes](https://raw.githubusercontent.com/george7378/chunked-terrain/master/_img/3.png)
![Peaceful islands](https://raw.githubusercontent.com/george7378/chunked-terrain/master/_img/4.png)
