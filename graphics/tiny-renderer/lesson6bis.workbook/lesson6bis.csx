using static Geometry;
using static MatrixHelpers;
using static ShaderUtils;

class TangentShader : IShader
{
    // triangle uv coordinates, written by the vertex shader, read by the fragment shader
    Vec3f varyingU = new Vec3f ();
    Vec3f varyingV = new Vec3f ();

    // normal per vertex to be interpolated by FS
    Matrix3 varyingNrm = new Matrix3 ();

    readonly Model model;
    readonly Vec3f lightDir;
    readonly Matrix4 uniformM;
    readonly Matrix4 uniformMIT;

    readonly Matrix4 transformation;
    readonly Vec3f[] ndcTri = new Vec3f[3];     // triangle in normalized device coordinates

    readonly Image texture;
    readonly Image normalMap;

    public TangentShader (Model model, Matrix4 viewport, Matrix4 projection, Matrix4 modelView, Vec3f lightDir, Image texture, Image normalMap)
    {
        this.model = model;
        this.lightDir = lightDir.Normalize ();
        this.texture = texture;
        this.normalMap = normalMap;

        uniformM = projection * modelView;
        uniformMIT = TransposeInverse (uniformM);
        transformation = viewport * uniformM;
    }

    public Vec4f Vertex (Face face, int nthvert)
    {
        UpdateVarayingUV (model, face, nthvert, ref varyingU, ref varyingV);

        var normal = Project3D (Mult (uniformMIT, Embed4D (model.Normal (face, nthvert))));
        varyingNrm.SetColumn (nthvert, normal);

        var glVertex = TransformFace (model, face, nthvert, transformation);
        ndcTri[nthvert] = Project3D (glVertex / glVertex.h);

        return glVertex;
    }

    public bool Fragment (Vec3f fragment, Vec3f bar, out Color color)
    {
        var bn = Mult(varyingNrm, bar).Normalize ();
        var uv = CalcUV (varyingU, varyingV, bar);

        var A = new Matrix3 ();
        A.SetRow (0, ndcTri [1] - ndcTri [0]);
        A.SetRow (1, ndcTri [2] - ndcTri [0]);
        A.SetRow (2, bn);

        var AI = Inverse (A);
        var i = Mult (AI, new Vec3f { x = varyingU.y - varyingU.x, y = varyingU.z - varyingU.x });
        var j = Mult (AI, new Vec3f { x = varyingV.y - varyingV.x, y = varyingV.z - varyingV.x });

        var B = new Matrix3 ();
        B.SetColumn (0, i.Normalize ());
        B.SetColumn (1, j.Normalize ());
        B.SetColumn (2, bn);

        var n = Mult (B, Normal (normalMap, uv)).Normalize ();

        var l = Transform (uniformM, lightDir).Normalize ();
        var diff = Math.Max (0f, Dot (n, l));

        color = GetColor (texture, uv) * diff;
        return false;
    }
}