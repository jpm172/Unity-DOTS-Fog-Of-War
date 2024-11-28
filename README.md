Two implementations of Fog of War, one scene demonstrates using a series of render textures, and one scene demonstrates using a stencil test.

| Render Texture Fog of War                 |Stencil Test Fog of War                   |
|-------------------------------------------|------------------------------------------|
| ![Render Texture Demo](images/rtGif.gif)  |  ![Stencil Demo](images/stencilGif.gif)  |

* The field of view alogrithm was taken from [Sebastian Lague's](https://www.youtube.com/watch?v=73Dc5JTCmKI&t=1s) series, and then adapated to work within the ECS framework
* The character controller was taken from [Poke Dev's](https://www.youtube.com/watch?v=YR6Q7dUz2uk&t=457s) video, and then adapated to work within the ECS framework

## Relevant Systems

* **EyeSystem**: Creates a procedural mesh for each eye that represents its field of view by casting a series of rays.
* **StencilEyeSystem**: Functions the same as **EyeSystem**, but skips updating any materials on the eye since they are not used in the stencil fog of war.
* **InitializeEyeSystem**: Creates the required meshes and materials for each new eye before they are updated in **EyeSystem/StencilEyeSystem**.
* **ObstacleCameraSystem**: Renders the output of and then disables the Obstacle Camera.

  
## Relevant Components

* **InitializeTag**: Attached to the eye prefab and used by the **InitializeEyeSystem** to grab any newly created eyes, and is removed after the initialization process.
* **EyeComponent**: Holds all the variables associated with the eye, like resolution, FOV, view distance, etc.
* **StencilEyeComponent**: Has all the information of **EyeComponent** minus all variables related to materials that are only used for the Render Texture FOW.
