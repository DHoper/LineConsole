using LineConsole.Domain.Entities;

namespace LineConsole.Application.RichMenus.Interfaces;

/// <summary>
/// ���ѥ��a Rich Menu �P�Ƶ{��ƪ��s���ާ@�]�ȭ���Ʈw�A���t LINE API�^
/// </summary>
public interface IRichMenuRepository
{
    // === Rich Menu �D�� ===

    /// <summary>
    /// �s�W�@�� Rich Menu �D���ơ]�]�t�I���ϰ�^
    /// </summary>
    Task AddAsync(RichMenu menu, CancellationToken ct = default);

    /// <summary>
    /// �d�߫��w LINE �x��b�����U�Ҧ� Rich Menu ��ơ]�]�t�I���ϰ�^
    /// </summary>
    Task<List<RichMenu>> GetAllByAccountAsync(Guid lineOfficialAccountId, CancellationToken ct = default);

    /// <summary>
    /// �ھ� RichMenuId �d�߳浧��ơ]�]�t�I���ϰ�^
    /// </summary>
    Task<RichMenu?> GetByIdAsync(Guid richMenuId, CancellationToken ct = default);

    /// <summary>
    /// �R�����w Rich Menu ��ơ]�P�ɧR�������ϰ��ơ^
    /// </summary>
    Task DeleteAsync(Guid richMenuId, CancellationToken ct = default);

    // === �Ƶ{�]Schedule�^ ===

    /// <summary>
    /// �s�W�@���Ƶ{��ơA���w�Y�ɶ��q���N�Y Rich Menu �]���w�]���
    /// </summary>
    Task AddScheduleAsync(Guid lineOfficialAccountId, Guid richMenuId, DateTime startTime, DateTime endTime, CancellationToken ct = default);

    /// <summary>
    /// �d�߬Y�b���ثe�|������B�b���w�ɶ������Ƶ{�]�Ω�I���Ƶ{�B�z�^
    /// </summary>
    Task<List<RichMenuSchedule>> GetActiveSchedulesAsync(Guid lineOfficialAccountId, DateTime now, CancellationToken ct = default);
}
