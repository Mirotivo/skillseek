
  export interface Listing {
    id: number;
    tutorId: number;
    name: string;
    contactedCount: number;
    reviews: number;
    category: string;
    title: string;
    image: string;
    lessonsTaught: string;
    location: string;
    locations: string[];
    aboutLesson: string;
    aboutYou: string;
    rate: string;
    rates: {
      hourly: string;
      fiveHours: string;
      tenHours: string;
    };
    socialPlatforms: string[];
  }
  