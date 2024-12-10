
  export interface Listing {
    id: number;
    tutorId: number;
    name: string;
    contactedCount: number;
    reviews: number;
    lessonCategoryId: number;
    category: string;
    title: string;
    image: string;
    imageFile: File;
    lessonsTaught: string;
    location: string;
    locations: string[];
    aboutLesson: string;
    aboutYou: string;
    rates: {
      hourly: number;
      fiveHours: number;
      tenHours: number;
    };
    socialPlatforms: string[];
  }
  