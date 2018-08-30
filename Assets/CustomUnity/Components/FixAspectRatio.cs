using UnityEngine;

namespace CustomUnity
{
	[RequireComponent(typeof(Camera))]
    [ExecuteInEditMode]
	public class FixAspectRatio : MonoBehaviour
	{
		public Vector2 aspectRatio = new Vector2(16, 9);
		Camera _camera = null;

		void Start()
		{
			_camera = GetComponent<Camera>();
		}

		void Update()
		{
			var targetAspectRatio = aspectRatio.x / aspectRatio.y;
			var currentAspectRatio = Screen.width / (float)Screen.height;
			var ratio = currentAspectRatio / targetAspectRatio;
			var pixelSize = currentAspectRatio / (ratio < 0.0f ? Screen.width : Screen.height) / targetAspectRatio;

			// 表示領域の横幅・高さ・左上のXY座標をセット
			// 目標より横長の場合
			if (ratio < 1.0f - pixelSize * 2) {
				var rect = _camera.rect;
				rect.x = 0.0f;
				rect.width = 1.0f;
				rect.y = (1.0f - ratio) / 2.0f;
				rect.height = ratio;
				_camera.rect = rect;
			}
            // 目標より縦長の場合
            else if (ratio > 1.0f + pixelSize * 2) {
				ratio = 1.0f / ratio;
				var rect = _camera.rect;
				rect.x = (1.0f - ratio) / 2.0f;
				rect.width = ratio;
				rect.y = 0.0f;
				rect.height = 1.0f;
				_camera.rect = rect;
			}
			// 元からあっている場合
			else {
				var rect = _camera.rect;
				rect.x = 0.0f;
				rect.width = 1.0f;
				rect.y = 0.0f;
				rect.height = 1.0f;
				_camera.rect = rect;
			}
		}
	}
}