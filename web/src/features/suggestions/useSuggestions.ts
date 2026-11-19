import { useQuery } from '@tanstack/react-query'
import { api } from '../../api'

/** Recipes that use up what's about to expire, best first. */
export function useSuggestions() {
  return useQuery({ queryKey: ['suggestions'], queryFn: api.suggestions })
}
