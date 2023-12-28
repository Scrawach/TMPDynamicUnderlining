using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TMPDynamicUnderline : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _text;
    [SerializeField] private string _linkTag;
    [SerializeField] private Image _underlineTemplate;

    [Tooltip("Height offset from text for underline, in pixels.")]
    public float HeightOffset = 1;
    
    [Tooltip("This time for every single underlining in text.")]
    public float TimeInSeconds = 1;
    
    private readonly List<GameObject> _underlines = new List<GameObject>();
    private Coroutine _coroutine;

    public void Play()
    {
        Stop();

        var points = CornerPoints(_text.textInfo, _linkTag).ToArray();
        _coroutine = StartCoroutine(Underlining(points, TimeInSeconds));
    }

    public void Stop()
    {
        if (_coroutine != null)
            StopCoroutine(_coroutine);
        _coroutine = null;

        foreach (var underline in _underlines) 
            Destroy(underline);
        _underlines.Clear();
    }

    private IEnumerator Underlining(IReadOnlyList<Vector3> points, float timeInSeconds)
    {
        for (var i = 0; i < points.Count; i += 2)
            yield return Underlining(points[i], points[i + 1], timeInSeconds);
    }

    private IEnumerator Underlining(Vector3 start, Vector3 end, float timeInSeconds)
    {
        var underline = Instantiate(_underlineTemplate, _text.transform);
        _underlines.Add(underline.gameObject);
        underline.rectTransform.localPosition = start;

        var initialPoint = underline.rectTransform.localPosition - new Vector3(0, HeightOffset, 0);
        var lineLength = end.x - start.x;
        var progress = 0f;

        while (progress < timeInSeconds)
        {
            var width = Mathf.Lerp(0, lineLength, progress / timeInSeconds);
            underline.rectTransform.sizeDelta = new Vector2(width, underline.rectTransform.sizeDelta.y);
            underline.rectTransform.localPosition = initialPoint + Vector3.right * width / 2;
            progress += Time.deltaTime;
            yield return null;
        }
    }

    private static IEnumerable<Vector3> CornerPoints(TMP_TextInfo textInfo, string linkTag)
    {
        var firstCharacterOnLineIndexes = textInfo.lineInfo.Select(line => line.firstCharacterIndex).ToHashSet();
        var linkInfos = textInfo.linkInfo;
        
        foreach (var linkInfo in linkInfos.Where(info => info.GetLinkID() == linkTag))
        {
            var startIndex = linkInfo.linkTextfirstCharacterIndex;
            var endIndex = linkInfo.linkTextfirstCharacterIndex + linkInfo.linkTextLength;
            var firstCharacter = textInfo.characterInfo[startIndex];
            var previousBottomRightPoint = firstCharacter.bottomRight;

            yield return firstCharacter.bottomLeft;
            
            for (var i = startIndex + 1; i < endIndex - 1; i++)
            {
                var character = textInfo.characterInfo[i];

                if (firstCharacterOnLineIndexes.Contains(i))
                {
                    yield return previousBottomRightPoint;
                    yield return character.bottomLeft;
                }

                previousBottomRightPoint = character.bottomRight;
            }

            yield return textInfo.characterInfo[endIndex - 1].bottomRight;
        }
    }
}
