import { useQuery, UseQueryOptions } from "@tanstack/react-query";
import { fetchProducts } from "../utils/api";
import { Products } from "../utils/models";

const useProducts = (options: { category: string }) => {
  const { category } = options;

  const queryKey = ["products", category];

  const fetchProductsByCategory = async () => {
    const response = await fetchProducts();
    if (category === "all") {
      return response;
    }
    return response.filter(
      (product) => product.category.toLowerCase() === category
    );
  };

  const queryOptions: UseQueryOptions<Products[], Error> = {
    queryKey,
    queryFn: fetchProductsByCategory,
    staleTime: 1000 * 60 * 60 * 6,
    refetchOnWindowFocus: false,
  };

  return useQuery<Products[], Error>(queryOptions);
};

export default useProducts;
