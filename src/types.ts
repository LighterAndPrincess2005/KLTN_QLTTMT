export type CourseCategory = 'Nền tảng' | 'Hội họa' | 'Thủ công';
export type ClassStatus = 'Chưa mở đăng ký' | 'Đang nhận đăng ký' | 'Đủ chỗ' | 'Đã khóa đăng ký' | 'Đang học' | 'Đã kết thúc';

export type Course = { id:string; title:string; ageMin:number; ageMax?:number; level:string; sessions:number; minutesPerSession:number; maxStudents:number; tuitionFee:number; materialFee:number; category:string; color:string; image:string; skills:string[]; prerequisites:string[] };
export type ArtClass = { id:string; code?:string; title?:string; tuitionFee?:number; deadline?:string; courseId:string; schedule:string; startDate:string; startDateISO?:string; registrationDeadline:string; teacher:string; room:string; capacity:number; enrolled:number; mode:'Trực tiếp'|'Trực tuyến'; status:ClassStatus };
export type StudentProfile = { id:string; name:string; birthDate:string; level:string; completedCourseIds:string[]; existingSchedules:string[]; verified?:boolean };
export type RegistrationDraft = { memberName:string; guardianName:string; phone:string; email:string; studentId:string; studentName:string; birthDate:string; level:string; courseId:string; classId:string; materialOption:'included'|'extra'|'none'; promotionCode:string; note:string };
export type EligibilityCheck = { label:string; passed:boolean; detail:string };
export type LearningRecord = { classId:string; courseTitle:string; schedule:string; attendance:{attended:number;absent:number;total:number}; makeUpRequests:{id:string;session:string;status:string}[]; teacherComment:string; products:string[]; progress:number; completionStatus:string; certificateEligible:boolean };
