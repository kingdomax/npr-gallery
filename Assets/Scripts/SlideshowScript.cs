using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

namespace NprGallery
{
    public class SlideshowScript : MonoBehaviourPunCallbacks
    {
        public RawImage slideshow;
        public GameObject buttonLeft;
        public GameObject buttonRight;
        public Texture[] slides;

        private int currentSlide = 0;

        void Start()
        {
            buttonLeft.SetActive(false);
            slideshow.texture = slides[currentSlide];
        }

        public void NavigateSlide(string tagName)
        {
            photonView.RPC("NavigateSlide", RpcTarget.AllBuffered, tagName);
        }

        [PunRPC]
        public void NavigateSlide(string tagName, PhotonMessageInfo info)
        {
            if (tagName == "ButtonLeft") { PreviousSlide(); }
            else if (tagName == "ButtonRight") { NextSlide(); }
        }

        private void PreviousSlide()
        {
            currentSlide--;

            buttonRight.SetActive(true);
            if (currentSlide == 0)
            {
                buttonLeft.SetActive(false);
            }

            slideshow.texture = slides[currentSlide];
        }

        private void NextSlide()
        {
            currentSlide++;

            buttonLeft.SetActive(true);
            if (currentSlide == slides.Length-1)
            {
                buttonRight.SetActive(false);
            }

            slideshow.texture = slides[currentSlide];
        }
    }
}
