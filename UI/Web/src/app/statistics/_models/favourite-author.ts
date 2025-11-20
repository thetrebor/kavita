import {Chapter} from "../../_models/chapter";

export type FavouriteAuthor = {
  authorId: number;
  authorName: string;
  totalChaptersRead: number;
  chapters: Chapter[];
}
