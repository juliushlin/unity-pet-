using UnityEngine;

// Original painted character on a deformable mesh: 2.5D motion, not a replacement 3D doll.
public class XiaoyueSpriteAvatar:MonoBehaviour
{
    public Texture2D portrait;
    public float LookDown {get;private set;}
    Mesh mesh;Vector3[] rest,vertices;Vector2[] uv;int shown=-1;float gazeVelocity;
    const int Columns=12,Rows=20;
    public void Initialize()
    {
        if(!portrait)portrait=Resources.Load<Texture2D>("XiaoyuePortrait");
        if(!portrait)throw new System.InvalidOperationException("小月原画未找到。");
        float width=5f*(portrait.width/3f)/(portrait.height/2f);rest=new Vector3[(Columns+1)*(Rows+1)];vertices=new Vector3[rest.Length];uv=new Vector2[rest.Length];
        for(int y=0;y<=Rows;y++)for(int x=0;x<=Columns;x++)rest[y*(Columns+1)+x]=new Vector3((x/(float)Columns-.5f)*width,(y/(float)Rows-.5f)*5,0);
        int[] triangles=new int[Columns*Rows*6];int t=0;for(int y=0;y<Rows;y++)for(int x=0;x<Columns;x++){int a=y*(Columns+1)+x;triangles[t++]=a;triangles[t++]=a+Columns+1;triangles[t++]=a+1;triangles[t++]=a+1;triangles[t++]=a+Columns+1;triangles[t++]=a+Columns+2;}
        mesh=new Mesh{name="XiaoyueOriginalArtMotion"};mesh.vertices=rest;mesh.triangles=triangles;mesh.uv=uv;var colors=new Color[rest.Length];for(int i=0;i<colors.Length;i++)colors[i]=Color.white;mesh.colors=colors;
        var filter=gameObject.AddComponent<MeshFilter>();filter.sharedMesh=mesh;var renderer=gameObject.AddComponent<MeshRenderer>();renderer.sharedMaterial=Resources.Load<Material>("PetSprite");var block=new MaterialPropertyBlock();block.SetTexture("_MainTex",portrait);renderer.SetPropertyBlock(block);
        var hit=gameObject.AddComponent<BoxCollider>();hit.size=new Vector3(width,5,.2f);SetFrame(0);
    }
    void SetFrame(int frame){if(frame==shown)return;shown=frame;for(int y=0;y<=Rows;y++)for(int x=0;x<=Columns;x++)uv[y*(Columns+1)+x]=new Vector2((frame%3+x/(float)Columns)/3f,(1-frame/3+y/(float)Rows)/2f);mesh.uv=uv;}
    public void Animate(PetMotion.Mode mode,int pose,float age,float time,bool chatOpen,float delta)
    {
        if(!mesh)return;SetFrame(pose);LookDown=Mathf.SmoothDamp(LookDown,chatOpen?1:0,ref gazeVelocity,.22f,10,delta);
        float angle=-LookDown*.25f+Mathf.Sin(time*1.5f)*.009f;
        for(int i=0;i<rest.Length;i++){Vector3 p=rest[i];float head=Mathf.SmoothStep(0,1,(p.y+.55f)/1.15f);float neck=p.y+.45f;float a=angle*head;p.y=-.45f+neck*Mathf.Cos(a);p.z=neck*Mathf.Sin(a);float hair=Mathf.SmoothStep(.5f,1.4f,Mathf.Abs(p.x))*Mathf.Clamp01((1.5f-p.y)/3);p.x+=Mathf.Sin(time*1.5f+rest[i].y*.8f)*hair*.045f;vertices[i]=p;}
        mesh.vertices=vertices;mesh.RecalculateBounds();
        float opening=mode==PetMotion.Mode.Opening?Mathf.Clamp01(age/.65f):1;float pop=Mathf.Lerp(.6f,1,1-Mathf.Pow(1-opening,3));
        float jump=pose==2?Mathf.Abs(Mathf.Sin(age*5))*Mathf.Exp(-age)*.45f:0;
        transform.localPosition=new Vector3(0,1.35f+Mathf.Sin(time*1.5f)*.025f+jump-LookDown*.045f,0);
        transform.localRotation=Quaternion.Euler(-LookDown*2,Mathf.Sin(time*.75f)*2.5f,Mathf.Sin(time*1.5f)*.8f+(pose==3?2:0));
        float breath=1+Mathf.Sin(time*1.5f)*.008f;transform.localScale=new Vector3(pop/breath,pop*breath,1);
    }
    void OnDestroy(){if(mesh){if(Application.isPlaying)Destroy(mesh);else DestroyImmediate(mesh);}}
}

