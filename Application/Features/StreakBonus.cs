using Core.Entities;

namespace Application.Features
{
  public class StreakBonus
  {
    public double CalculateStreakBonus(List<Submission> submissions)
    {
      try
      {

        submissions = submissions.OrderBy(s => s.SubmittedAt).ToList();

        int currentStreak = 0;
        int streakScore = 0;
        var bonus = 0.0;
        const double maxBonus = 5.0;

        foreach (var submission in submissions)
        {
          var quiz = submission.Question.Quiz;

          var totalQuestions = quiz.Questions.Count;
          var expectedTimePerQuestion = (quiz.EndTime - quiz.StartTime).TotalSeconds / totalQuestions;

          var orderedQuestions = quiz.Questions.OrderBy(q => q.ID).ToList();
          var questionIndex = orderedQuestions.FindIndex(q => q.ID == submission.QuestionID);

          var elapsedSeconds = (submission.SubmittedAt - quiz.StartTime).TotalSeconds;
          var previousQuestionsTime = questionIndex * expectedTimePerQuestion;
          var timeIntoCurrentQuestion = elapsedSeconds - previousQuestionsTime;

          var ratio = timeIntoCurrentQuestion / expectedTimePerQuestion;
          bonus += Math.Max(0, maxBonus * (1 - ratio));

          bool isCorrect = submission.Question.Options
              .FirstOrDefault(o => o.ID == submission.SelectedOptionId)?.IsCorrect ?? false;
          if (isCorrect)
          {
            currentStreak++;
          }
          else
          {
            streakScore += currentStreak * 5;
            currentStreak = 0;
          }
        }
        return (streakScore + currentStreak * 5 + bonus);
      }
      catch (Exception ex)
      {
        return 0.0;
      }
    }
  }
}
