using Photon.Pun;
using UnityEngine;
using NprGallery.Models;

namespace NprGallery
{
    public class PaintHandler : MonoBehaviourPunCallbacks
    {
        private RenderingTechnique _currentRenderingTechnique;
        private Material _defaultMat;
        private Mesh _defaultMesh;

        void Start()
        {
            _currentRenderingTechnique = RenderingTechnique.Default;
            _defaultMat = new Material(GetComponent<MeshRenderer>().material);
            _defaultMesh = GetComponent<MeshFilter>().mesh;
        }

        public void OnPaint(RenderingTechnique newRenderingTechnique)
        {
            if (newRenderingTechnique != _currentRenderingTechnique)
            {
                photonView.RPC("OnPaint", RpcTarget.AllBuffered, newRenderingTechnique);
            }
        }
        
        [PunRPC]
        public void OnPaint(RenderingTechnique newRenderingTechnique, PhotonMessageInfo info)
        {
            _currentRenderingTechnique = newRenderingTechnique;

            ResetToDefault(); // make sure we always reset material and mesh to default, before aplying new rendering technique

            var config = RenderingProvider.GetRenderingConfig(newRenderingTechnique);
            switch (config.Technique)
            {
                case RenderingTechnique.Toon: 
                    ApplyToonShading((ToonShadingConfig) config);
                    break;
                case RenderingTechnique.Gooch:
                    ApplyGoochShading((GoochShadingConfig) config);
                    break;
                case RenderingTechnique.DuotoneSurface:
                    ApplyDuotoneSurface((DuotoneConfig) config);
                    break;
                case RenderingTechnique.RealtimeHatching:
                    ApplyRealtimeHatching((RealtimeHatchingConfig) config);
                    break;
                case RenderingTechnique.Default:
                default: break;
            }
        }

        private void ResetToDefault()
        {
            GetComponent<MeshRenderer>().material = new Material(_defaultMat);
            GetComponent<MeshFilter>().mesh = _defaultMesh;
        }

        private void ApplyToonShading(ToonShadingConfig config)
        {
            var currentMaterial = GetComponent<MeshRenderer>().material;
            currentMaterial.shader = Shader.Find(config.ShaderFile);
            currentMaterial.SetFloat("_Glossiness", config.Glossiness);         
        }

        private void ApplyGoochShading(GoochShadingConfig config)
        {
            var currentMaterial = GetComponent<MeshRenderer>().material;
            currentMaterial.shader = Shader.Find(config.ShaderFile);
        }

        private void ApplyRealtimeHatching(RealtimeHatchingConfig config)
        {
            GetComponent<Renderer>().material = Resources.Load("RealtimeHatching", typeof(Material)) as Material;
        }

        private void ApplyDuotoneSurface(DuotoneConfig config)
        {
            GetComponent<Renderer>().material = Resources.Load("DuotonMaterial", typeof(Material)) as Material;
            GameObject dividedPrefab = Resources.Load("DuotonePrefabs/" + this.name) as GameObject;
            GameObject objectFromPrefab = Instantiate(dividedPrefab, new Vector3(0, 0, 0), Quaternion.identity);

            GetComponent<MeshFilter>().mesh = objectFromPrefab.GetComponent<MeshFilter>().mesh;

            Destroy(objectFromPrefab);
            //can this be performed async?
            //Mesh mesh = GetComponent<MeshFilter>().mesh;
            //mesh = Subdivider.getSubdividedMesh(mesh, 1, 1, true); ;
        }
    }
}
