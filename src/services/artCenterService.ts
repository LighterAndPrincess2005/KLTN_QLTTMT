import { classes, courses } from '../data/mockData';
import type { RegistrationDraft } from '../types';

const delay = (ms = 180) => new Promise((resolve) => setTimeout(resolve, ms));

export const artCenterService = {
  async getCourses() { await delay(); return courses; },
  async getClasses() { await delay(); return classes; },
  async submitRegistration(payload: RegistrationDraft) {
    await delay(500);
    return { id: `DK-${Date.now().toString().slice(-6)}`, status: 'CHỜ THANH TOÁN', holdUntil: '48 giờ kể từ lúc đăng ký', payload };
  },
};
