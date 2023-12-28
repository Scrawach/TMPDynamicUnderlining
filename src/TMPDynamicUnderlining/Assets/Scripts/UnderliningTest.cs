using UnityEngine;

public class UnderliningTest : MonoBehaviour
{
    [SerializeField] private TMPDynamicUnderline _underline;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            _underline.Play();
    }
}