using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace LineConsole.Application.RichMenus.Models;

/// <summary>
/// �إ� Rich Menu �äW�ǹϤ����ШD��ơ]multipart/form-data�^
/// </summary>
public class CreateRichMenuInput
{
    /// <summary>�O�_���w�]���</summary>
    public bool Selected { get; init; }

    /// <summary>�Ϥ���W��</summary>
    [Required]
    public string Name { get; init; } = string.Empty;

    /// <summary>Chat bar ��ܤ�r</summary>
    [Required]
    public string ChatBarText { get; init; } = string.Empty;

    /// <summary>�ؤo��T�]�e�סB���ס^</summary>
    public MenuSize Size { get; init; } = default!;

    /// <summary>�I���ϰ�M��</summary>
    public List<MenuArea> Areas { get; init; } = new();

    /// <summary>�Ϥ��ɮס]JPEG/PNG�^</summary>
    [Required]
    public IFormFile Image { get; init; } = null!;

    /// <summary>�Ƶ{�}�l�ɶ��]���^</summary>
    public DateTime? ScheduleStart { get; init; }

    /// <summary>�Ƶ{�����ɶ��]���^</summary>
    public DateTime? ScheduleEnd { get; init; }
}
