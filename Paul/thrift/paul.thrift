namespace csharp Paul

struct Work {
  1: i32 num1 = 0,
  2: i32 num2 = 0,
  3: optional string comment,
}

service Service{
        list<Work> checkcalc(1:i32 a, 2:i32 b)
}
