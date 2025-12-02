using UnityEngine;
using UnityEngine.UI;

public class MyPageCategoryButton : MonoBehaviour
{
    public string category;

    private void Start()
    {
        GetComponent<Button>().onClick.AddListener(() =>
        {
            MyPageManager.Instance.ChangeCategory(category);
        });
    }
}
