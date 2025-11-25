using UnityEngine;
using UnityEngine.UI;

public class StoreCategoryButton : MonoBehaviour
{
    public string category;

    private void Start()
    {
        GetComponent<Button>().onClick.AddListener(() =>
        {
            StoreManager.Instance.ChangeCategory(category);
        });
    }
}
