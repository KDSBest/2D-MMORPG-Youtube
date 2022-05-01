using UnityEngine;

namespace Assets.Scripts.Prop
{
    public class EnemyBehaviour : MonoBehaviour
    {
		public int HP { get; set; } = 900;
		public int MaxHP { get; set; } = 1000;
		public string Name { get; set; }
        public SpriteRenderer Renderer;
        private Material material;

		public void Awake()
		{
			material = Renderer.material;
		}

		public void Update()
		{
			material.SetInt("Health", HP);
			material.SetInt("MaxHealth", MaxHP);
		}
	}
}