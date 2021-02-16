using OpenCvSharp;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ZTuber : MonoBehaviour
{
	public RawImage rawImage;
	private WebCamTexture webCamTexture; // Live VideのTexture
	private WebCamDevice[] devices;
	private int selectCamera = 0;

	private CascadeClassifier cascadeFace;
	private Texture2D outTexture;

	private Mat inputMat;
	private Mat grayMat;

	private int width = 1920;
	private int height = 1080;
	private int fps = 30;

	void Start()
	{
		StartCoroutine("RequestCamera");
		devices = WebCamTexture.devices;
		for (int i = 0; i < devices.Length; i++) { Debug.Log($"Device{i} Name: {devices[i].name}"); }

		webCamTexture = new WebCamTexture(devices[1].name, width, height, fps);
		Debug.Log($"web cam texture name: {webCamTexture.name}");
		webCamTexture.Play();
		cascadeFace = new CascadeClassifier($"{Application.dataPath}/MyScripts/haarcascade_frontalface_default.xml");
	}

	void LateUpdate()
	{
		ExtractFaceCenter(webCamTexture);
	}

	private void ExtractFaceCenter(WebCamTexture webCamTex)
	{
		// OpenCV形式Matに変換し、グレースケール
		inputMat = OpenCvSharp.Unity.TextureToMat(webCamTex);
		grayMat = new Mat();
		Cv2.CvtColor(inputMat, grayMat, ColorConversionCodes.BGR2GRAY);

		/* 顔検出
		 * Mat image: CV_8U 型の入力行列
		 * doubule scaleFactor = 1.1: 各画像スケールにおける縮小量
		 * int minNeighbors = 3: 物体候補となる矩形は，最低でもこの数だけの近傍矩形を含む必要がある
		 * HaarDetectionType flags = 0: このパラメータは，新しいカスケードでは利用されません．古いカスケードに対しては，cvHaarDetectObjects 関数の場合と同じ意味を持ちます
		 * Size? minSize: 物体が取り得る最小サイズ: 性能にめちゃ影響
		 * Size? maxSize: 物体が取り得る最大サイズ
		 */
		OpenCvSharp.Rect[] rectFaces = cascadeFace.DetectMultiScale(image: grayMat, scaleFactor: 1.1, minNeighbors: 3, 0, minSize: new Size(100, 100));

		if (rectFaces.Length > 0)
		{
			foreach (OpenCvSharp.Rect rectFace in rectFaces)
			{
				Debug.Log($"Left: {rectFace.BottomLeft.X}, Right: {rectFace.BottomRight.X}");
				grayMat.Rectangle(rectFace.TopLeft, rectFace.BottomRight, new Scalar(0, 255, 0, 255));
				break;
			}
			renderDebugFace(grayMat);
		}
	}

	// for Debug
	private void renderDebugFace(Mat mat)
	{
		outTexture = OpenCvSharp.Unity.MatToTexture(mat);
		try
		{
			rawImage.texture = outTexture;
		}
		catch (System.NullReferenceException e)
		{
			Debug.Log($"Null po: {e}");
		}
	}

	private IEnumerator RequestCamera()
	{
		yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);
		if (!Application.HasUserAuthorization(UserAuthorization.WebCam))
		{
			Debug.LogFormat("カメラを使うことが許可されていません");
			yield break;
		}
	}
}
