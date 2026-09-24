export type Course = {
  id: string;
  title: string;
  age: string;
  duration: string;
  sessions: number;
  price: number;
  materialFee: number;
  category: 'Nền tảng' | 'Hội họa' | 'Thủ công';
  color: string;
  image: string;
  skills: string[];
};

export type ArtClass = {
  id: string;
  courseId: string;
  schedule: string;
  startDate: string;
  teacher: string;
  room: string;
  capacity: number;
  enrolled: number;
  mode: 'Trực tiếp' | 'Trực tuyến';
};

export type RegistrationDraft = {
  courseId: string;
  classId: string;
  studentName: string;
  birthDate: string;
  guardianName: string;
  phone: string;
  email: string;
  note: string;
};
