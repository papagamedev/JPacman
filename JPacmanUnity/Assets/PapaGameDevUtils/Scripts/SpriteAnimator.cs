using UnityEngine;
using System.Collections;

namespace PapaGameDev.Utils
{

	public class SpriteAnimator : MonoBehaviour
	{
		[SerializeField]
		Sprite[] m_animationFrames;

		[SerializeField]
		float m_animationLength = 1.0f;

		[SerializeField]
		WrapMode m_wrapMode;

		SpriteRenderer sprite;
		float m_startTime;
		bool m_backwards = false;

		void Start () {
			sprite = GetComponent<SpriteRenderer> ();
			if (sprite == null) {
				Debug.LogError ("SpriteAnimator requires a SpriteRenderer component in gameobject: "+ gameObject.name);
			}
			m_startTime = Time.time;
		}
		
		void Update () {

			float currentTime = UpdateAnimationTime ();
			int frame = UpdateAnimationFrame (currentTime);
			UpdateSpriteFrame (frame);
		}

		float UpdateAnimationTime()
		{
			float currentTime = Time.time - m_startTime;
			if (currentTime > m_animationLength) {

				switch(m_wrapMode)
				{
				case WrapMode.Default:
				case WrapMode.Once:
					currentTime = 0.0f;
					enabled = false;
					break;

				case WrapMode.ClampForever:
					enabled = false;
					break;
				
				case WrapMode.Loop:
					currentTime -= m_animationLength;
					m_startTime += m_animationLength;
					break;

				case WrapMode.PingPong:
					currentTime -= m_animationLength;
					m_startTime += m_animationLength;
					m_backwards = !m_backwards;
					break;
				}
			}

			return currentTime;
		}

		int UpdateAnimationFrame(float currentTime)
		{
			int frameCount = m_animationFrames.Length;
			if (m_wrapMode == WrapMode.PingPong) {
				frameCount--;
			}
			int frame = (int)(currentTime * frameCount / m_animationLength);
			if (frame >= frameCount) {
				frame = frameCount - 1;
			}
			if (m_backwards) {
				frame = frameCount - frame;
			}
			return frame;
		}

		void UpdateSpriteFrame(int frame)
		{
			sprite.sprite = m_animationFrames [frame];
		}
	}

} // namespace PapaGameDev.Utils
