using UnityEngine;

namespace Assets.Scripts.Prop
{
    public class PropBehaviour : MonoBehaviour
    {
		public int Health { get; set; } = 900;
		public int MaxHealth { get; set; } = 1000;
		public string Name { get; set; }
        public SpriteRenderer Renderer;
        private Material material;

		public void Awake()
		{
			material = Renderer.material;
		}

		public void Update()
		{
			material.SetInt("Health", Health);
			material.SetInt("MaxHealth", MaxHealth);
		}
	}
}