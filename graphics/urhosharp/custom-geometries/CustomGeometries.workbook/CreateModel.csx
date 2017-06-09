using System;
using System.Collections.Generic;
using Urho;

Model CreateModel(IList<Vector3> positions,
                  IList<Vector3> normals = null,
                  IList<Color> colors = null,
                  IList<Vector2> texcoords = null)
{
    // Check that the positions argument isn't null
    if (positions == null)
        throw new ArgumentNullException("positions");

    // Make sure it has a property number of elements
    int vertexCount = positions.Count;

    if (vertexCount == 0 || vertexCount % 3 != 0)
        throw new ArgumentOutOfRangeException("positions count must be non-zero and a multiple of 3");

    // Check that the normals argument is consistent, or create it
    if (normals != null)
    {
        if (normals.Count != vertexCount)
            throw new ArgumentOutOfRangeException("texcoords must have same count as positions");
    }
    else
    {
        normals = new Vector3[vertexCount];

        for (int i = 0; i < vertexCount; i += 3)
        {
            normals[i + 0] = Vector3.Cross(positions[i + 1] - positions[i + 0],
                                            positions[i + 2] - positions[i + 0]);

            normals[i + 1] = Vector3.Cross(positions[i + 2] - positions[i + 1],
                                            positions[i + 0] - positions[i + 1]);

            normals[i + 2] = Vector3.Cross(positions[i + 0] - positions[i + 2],
                                            positions[i + 1] - positions[i + 2]);
        }
    }

    // Create the VertexBuffer object; set it in one of the blocks below
    VertexBuffer vertexBuffer = new VertexBuffer(Application.CurrentContext, false)
    {
        Shadowed = true
    };

    // If texcoords is non-null, use a PositionNormalColorTexcoord structure
    if (texcoords != null)
    {
        if (texcoords.Count != vertexCount)
            throw new ArgumentOutOfRangeException("texcoords must have same count as positions");

        if (colors != null && colors.Count != vertexCount)
            throw new Exception("colors must have same count as positions");

        var vertices = new VertexBuffer.PositionNormalColorTexcoord[vertexCount];

        for (int i = 0; i < vertexCount; i++)
        {
            vertices[i].Position = positions[i];
            vertices[i].Normal = normals[i];
            vertices[i].Color = colors == null ? 0 : colors[i].ToUInt();
            vertices[i].TexCoord = texcoords[i];
        }

        vertexBuffer.SetSize((uint)vertexCount, ElementMask.Position |
                                                ElementMask.Normal |
                                                ElementMask.Color |
                                                ElementMask.TexCoord1,
                                                false);
        vertexBuffer.SetData(vertices);
    }
    // If colors is non-null, use a PositionNormalColor structure
    else if (colors != null)
    {
        if (colors.Count != vertexCount)
            throw new Exception("colors must have same count as positions");

        VertexBuffer.PositionNormalColor[] vertices = new VertexBuffer.PositionNormalColor[vertexCount];

        for (int i = 0; i < vertexCount; i++)
        {
            vertices[i].Position = positions[i];
            vertices[i].Normal = normals[i];
            vertices[i].Color = colors[i].ToUInt();
        }

        vertexBuffer.SetSize((uint)vertexCount, ElementMask.Position | 
                                                ElementMask.Normal | 
                                                ElementMask.Color, false);
        vertexBuffer.SetData(vertices);
    }
    // Otherwise use the PositionNormal structure
    else
    {
        VertexBuffer.PositionNormal[] vertices = new VertexBuffer.PositionNormal[vertexCount];

        for (int i = 0; i < vertexCount; i++)
        {
            vertices[i].Position = positions[i];
            vertices[i].Normal = normals[i];
        }

        vertexBuffer.SetSize((uint)vertexCount, ElementMask.Position | 
                                                ElementMask.Normal, false);
        vertexBuffer.SetData(vertices);
    }

    // Create the Geometry object
    Geometry geometry = new Geometry();
    geometry.SetVertexBuffer(0, vertexBuffer);
    geometry.SetDrawRange(PrimitiveType.TriangleList, 0, 0, 0, (uint)vertexCount, true);

    // Create the Model object
    Model model = new Model();
    model.NumGeometries = 1;
    model.SetGeometry(0, 0, geometry);
    model.BoundingBox = new BoundingBox(new Vector3(-10, -10, -10), new Vector3(10, 10, 10));

    return model;
}
