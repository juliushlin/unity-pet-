using UnityEngine;
public class PencilIconGraphic:UnityEngine.UI.MaskableGraphic
{
    protected override void OnPopulateMesh(UnityEngine.UI.VertexHelper v)
    {
        v.Clear();var r=rectTransform.rect;Vector2 P(float x,float y)=>new Vector2(r.xMin+x*r.width/24,r.yMin+y*r.height/24);
        void Line(float x,float y,float u,float w){var a=P(x,y);var b=P(u,w);var n=new Vector2(-(b-a).y,(b-a).x).normalized*1.25f;int k=v.currentVertCount;v.AddVert(a+n,color,Vector2.zero);v.AddVert(b+n,color,Vector2.zero);v.AddVert(b-n,color,Vector2.zero);v.AddVert(a-n,color,Vector2.zero);v.AddTriangle(k,k+1,k+2);v.AddTriangle(k,k+2,k+3);}
        Line(8,21,5,21);Line(5,21,3,19);Line(3,19,3,5);Line(3,5,5,3);Line(5,3,19,3);Line(19,3,21,5);Line(21,5,21,9);
        Line(9,10,19,20);Line(12,7,22,17);Line(19,20,22,17);Line(9,10,8,6);Line(8,6,12,7);Line(17,18,20,15);
    }
}
