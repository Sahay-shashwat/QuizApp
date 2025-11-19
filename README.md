🎯 Quiz App – Feature Overview
The Quiz App allows administrators to create and manage quizzes while enabling players to participate in real-time question-answer rounds with dynamic scoring and a live leaderboard.

👨‍💼 Admin Features
  📝 Create & Manage Quizzes

  Create quizzes with title, start time, end time, and duration.

  Mark quizzes as active, scheduled, or completed.

  ❓ Add Questions

  Add multiple questions with:
    Options
    Correct answer
    Marks/weightage

  🚀 Publish Quiz

  Publish quizzes to make them available to players.
  
  Unpublished quizzes remain invisible to participants.
  
  📊 Monitor Live Leaderboard
  
  Track player performance in real time.

  See:
    Current ranks
    Total score
    Number of correct answers    
    Progress through questions

🎮 Player Features
📅 View Available Quizzes

See all active or upcoming quizzes.

Join quizzes once the scheduled start time is reached.

▶️ Start Quiz

Begin answering questions when the quiz becomes active.

🧠 Answer Questions

Submit an answer for each question.

Automatically moves to next question.

Optional progress display (e.g., Question 5/20).

🏁 Complete Quiz

Quiz ends when:

All questions are answered, or

Time runs out

📈 Final Score

View a performance summary including:

Total score

Number of correct answers

Time taken

🏆 Live Leaderboard

See real-time ranking updates as players submit answers.

Displays:

Current rank

Total score

Competitor scores

Status indicators (e.g., “Answering”, “Completed”)

🔒 Common Features
🔐 Authentication & Roles

All users log in using the same system.

Role-based routing:

Admins → quiz creation flow

Players → quiz-taking flow
<div>
  <h2>FLOW DIAGRAM</h2>
  ![Flow QuizApp](https://github.com/user-attachments/assets/f3d39c4f-1e0c-413d-8071-f379709b2710)
</div>
<div>
  <h2>Sequence Diagram - User attempting Quiz</h2>
  <img width="813" height="642" alt="Sequence Diagram" src="https://github.com/user-attachments/assets/8d851dd3-6eb6-48e9-81be-8c8ef378a37b" />
</div>
<div>
  <h2>FLOW DIAGRAM - LIVE LEADERBOARD</h2>
  ![Flow1](https://github.com/user-attachments/assets/a9c36beb-9359-4bb7-acea-080e91248b27)
</div>
