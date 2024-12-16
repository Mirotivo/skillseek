public interface ILessonService
{
    Task<LessonDto> ProposeLesson(LessonDto lessonDto, int userId);
    Task<bool> RespondToProposition(int lessonId, bool accept, int userId);
    Task<List<LessonDto>> GetPropositionsAsync(int contactId, int userId);
    Task<List<LessonDto>> GetLessonsAsync(int contactId, int userId);
    Task ProcessPaymentAsync(Lesson lesson);
}
