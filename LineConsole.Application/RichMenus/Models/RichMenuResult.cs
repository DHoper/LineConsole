namespace LineConsole.Application.RichMenus.Models;

/// <summary>�^�ǳ�@ Rich Menu �ԲӸ��</summary>
public record RichMenuResult
{
    public string RichMenuId { get; init; } = string.Empty; // RichMenuDTO ���� ID

    public string Name { get; init; } = string.Empty; // ���W��

    public string ChatBarText { get; init; } = string.Empty; // �}�Ҵ��ܤ�r

    public bool Selected { get; init; } // �O�_���w�]

    public MenuSize Size { get; init; } = default!; // �ؤo�]�e���^

    public List<MenuArea> Areas { get; init; } = new(); // �I���϶��M��
}

/// <summary>�^�� Rich Menu ID �����G</summary>
public record RichMenuIdResult
{
    public string RichMenuId { get; init; } = string.Empty; // RichMenuDTO ���� ID
}

/// <summary>�^�� Rich Menu �M�浲�G</summary>
public record RichMenuListResult
{
    public List<RichMenuResult> RichMenus { get; init; } = new(); // �Ҧ� RichMenuDTO ���G
}

/// <summary>�^�� Rich Menu�]�t�Ϥ� base64 �w���^</summary>
public record RichMenuWithImageResult
{
    public required string RichMenuId { get; init; } // RichMenuDTO ���� ID

    public required string Name { get; init; } // �W��

    public required int Width { get; init; } // �e

    public required int Height { get; init; } // ��

    public required bool Selected { get; init; } // �O�_���w�]

    public required string ChatBarText { get; init; } // ��ܤ�r

    public required string ImageBase64 { get; init; } // �w���� base64 �r��
}
