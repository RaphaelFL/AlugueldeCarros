export interface NotFoundPageViewModel {
  homePath: string;
}

export function useNotFoundPageLogic(): NotFoundPageViewModel {
  return { homePath: '/' };
}