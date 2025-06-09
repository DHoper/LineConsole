namespace LineConsole.Application.RichMenus.Models;

/// <summary>
/// �إߩ����� Rich Menu ���c�Ϊ���Ƽҫ��]�ǵ� LINE API�^
/// </summary>
public record RichMenuDTO
{
    public MenuSize Size { get; init; } = default!; // ���ؤo�]�e x ���^

    public bool Selected { get; init; } // �O�_���w�]���

    public string Name { get; init; } = string.Empty; // ���W��

    public string ChatBarText { get; init; } = string.Empty; // Chat bar ���ܤ�r

    public List<MenuArea> Areas { get; init; } = new(); // �I���ϰ�M��

    public DateTime? StartTime { get; init; } // �Ƶ{�_�l�ɶ�
    public DateTime? EndTime { get; init; }   // �Ƶ{�����ɶ�

}

/// <summary>
/// Rich Menu �ؤo��T
/// </summary>
public record MenuSize
{
    public int Width { get; init; } // �e��

    public int Height { get; init; } // ����
}

/// <summary>
/// �I���ϰ�϶��w�q
/// </summary>
public record MenuArea
{
    public MenuBounds Bounds { get; init; } = default!; // �I���y�нd��

    public MenuAction Action { get; init; } = default!; // �I������檺�欰
}

/// <summary>
/// �I���϶��y�нd��
/// </summary>
public record MenuBounds
{
    public int X { get; init; } // ���W�� X �y��

    public int Y { get; init; } // ���W�� Y �y��

    public int Width { get; init; } // �e��

    public int Height { get; init; } // ����
}

/// <summary>
/// �I���欰�w�q�]�� LINE API �W��^
/// </summary>
public record MenuAction
{
    public string Type { get; init; } = string.Empty; // �欰�����]�p postback�Buri�Bmessage ���^

    public string? Text { get; init; } // �Y�� message�A�ҭn��ܪ���r

    public string? Data { get; init; } // �Y�� postback�A�ҭn�e�X�� data

    public string? Uri { get; init; } // �Y�� uri�A�}�Ҫ����}

    public string? RichMenuAliasId { get; init; } // �Y�� richmenuswitch�A�������O�W ID

    public string? Datetime { get; init; } // �Y�� datetimepicker�AISO �榡�ɶ��r��
}
