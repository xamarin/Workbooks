using System;
using System.Collections.Generic;

using System.Reflection;
using Urho;

// Node lightNode;
// Light light;

void Initialize(SimpleApplication app)
{
    // Move the camera and set it's direction
    app.CameraNode.Position = new Vector3(0, 0, -5);

    app.CameraNode.SetDirection(new Vector3(0, 0, 1));

    // Set the root node at the origin
    app.RootNode.Position = new Vector3(0, 0, 0);

    // Remove the point light
    
app.CameraNode.RemoveAllChildren();



    // Remove previous directionalLight node before adding new one

    app.Scene.RemoveChild(app.Scene.GetChild("directionalLight"));
    
Node lightNode = app.Scene.CreateChild("directionalLight");

    

// Create the light compononent

    Light light = lightNode.CreateComponent<Light>();

    light.LightType = LightType.Directional;

    light.Color = new Color(0.6f, 0.6f, 0.6f);



    // Set the direction of the directional light

    lightNode.SetDirection(new Vector3(2, -3, 1));

    

// Set the ambient light
    
app.Zone.AmbientColor = new Color(0.4f, 0.4f, 0.4f);

    // Disable the code that moves the camera in response to mouse and touch
    app.MoveCamera = false;

    // Detach ALL Update handler
FieldInfo field = typeof(Application).GetField("Update", BindingFlags.Instance | BindingFlags.NonPublic);

    field.SetValue(app, null);

    // Define an event handler to apply rotation to RootNode with the mouse
    app.Update += (UpdateEventArgs args) =>
    {
    if (app.Input.GetMouseButtonDown(MouseButton.Left))

    {

        Vector2 mouseMove = new Vector2(app.Input.MouseMove.X, -app.Input.MouseMove.Y);

        float angle = mouseMove.Length;         // 1 degree per pixel is simple



        if (angle > 0)

        {

            Vector3 axis = new Vector3(mouseMove.Y, -mouseMove.X, 0);

            app.RootNode.Rotate(Quaternion.FromAxisAngle(axis, angle), TransformSpace.Parent);

        }

    }

    };

}

void ProcessMouseMovement(UpdateEventArgs args)

{

    if (app.Input.GetMouseButtonDown(MouseButton.Left))

    {

        Vector2 mouseMove = new Vector2(app.Input.MouseMove.X, -app.Input.MouseMove.Y);

        float angle = mouseMove.Length;         // 1 degree per pixel is simple



        if (angle > 0)

        {

            Vector3 axis = new Vector3(mouseMove.Y, -mouseMove.X, 0);

            app.RootNode.Rotate(Quaternion.FromAxisAngle(axis, angle), TransformSpace.Parent);

        }

    }

}