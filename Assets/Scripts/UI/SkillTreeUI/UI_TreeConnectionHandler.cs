using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


[Serializable]
public class UI_TreeConnectionDetails
{
    public UI_TreeConnectionHandler childNode;
    public NodeDirectionType direction;
    [Range(100f, 350f)] public float length;
    [Range(-25f, 25f)] public float rotation;
}

public class UI_TreeConnectionHandler : MonoBehaviour
{
    private RectTransform rect => GetComponent<RectTransform>();
    [SerializeField] private UI_TreeConnectionDetails[] connectionsDetails;
    [SerializeField] private UI_TreeConnection[] connections;

    private Image connectionImage;
    private Color originalColor;
    private bool originalColorCaptured;

    private void Awake()
    {
        connectionImage = GetComponent<Image>();
        if (connectionImage != null)
        {
            originalColor = connectionImage.color;
            originalColorCaptured = true;
        }
    }

    // OnValidate 中修改 RectTransform.sizeDelta 会触发子对象的
    // OnRectTransformDimensionsChange → SendMessage（禁止），
    // 用 EditorApplication.delayCall 延期执行以绕过该限制
#if UNITY_EDITOR
    private static readonly HashSet<UI_TreeConnectionHandler> pendingUpdates = new HashSet<UI_TreeConnectionHandler>();
#endif

    private void OnValidate()
    {
        if (connectionsDetails == null || connectionsDetails.Length <= 0) return;

        // 自动同步 connections 数组长度与 connectionsDetails 一致
        if (connections == null || connections.Length != connectionsDetails.Length)
        {
            Array.Resize(ref connections, connectionsDetails.Length);
            Debug.Log("已自动调整连线数量: " + gameObject.name);
        }

#if UNITY_EDITOR
        if (pendingUpdates.Contains(this)) return;
        pendingUpdates.Add(this);
        UnityEditor.EditorApplication.delayCall += () =>
        {
            pendingUpdates.Remove(this);
            if (this == null) return;
            UpdateConnections();
        };
#endif
    }

    public UI_TreeNode[] GetChildNodes()
    {
        List<UI_TreeNode> childNodes = new List<UI_TreeNode>();
        foreach (var detail in connectionsDetails)
        {
            if (detail.childNode != null)
            {
                var childNodeComponent = detail.childNode.GetComponent<UI_TreeNode>();
                if (childNodeComponent != null)
                {
                    childNodes.Add(childNodeComponent);
                }
            }
        }
        return childNodes.ToArray();
    }

    public void UpdateConnections()
    {
        for (int i = 0; i < connectionsDetails.Length; i++)
        {
            var detail = connectionsDetails[i];
            var connection = connections[i];
            if (connection == null) continue;

            Vector2 targetPosition = connection.GetConnectionPoint(rect);
            Image connectionImage = connection.GetConnectionImage();

            connection.DirectConnection(detail.direction, detail.length, detail.rotation);

            if (detail.childNode == null) continue;

            detail.childNode.SetPosition(targetPosition);
            detail.childNode.SetConnectionImage(connectionImage);
            //detail.childNode.transform.SetAsLastSibling();
        }
    }

    public void UpdateAllConnections()
    {
        UpdateConnections();

        foreach (var node in connectionsDetails)
        {
            if (node.childNode == null) continue;
            node.childNode?.UpdateConnections();
        }
    }

    public void ConnectionImageUnlocked(bool unlocked)
    {
        if (connectionImage == null) return;

        connectionImage.color = unlocked ? Color.white : originalColor;
    }

    public void SetConnectionImage(Image image)
    {
        connectionImage = image;
        // 只在第一次拿到连线 Image 时记录原始颜色，
        // 避免解锁后 UpdateConnections 再次执行时把白色当成原始色
        if (image != null && !originalColorCaptured)
        {
            originalColor = image.color;
            originalColorCaptured = true;
        }
    }
    public void SetPosition(Vector2 position) => rect.anchoredPosition = position;
}
