/* eslint-disable @typescript-eslint/no-explicit-any */

import { IColumn } from '@fluentui/react';

export const generateItemsFromSearch = (
  itemGroups: any[],
  columns: IColumn[]
): any[] => {
  const items: any[] = [];

  const keysFromColumns: string[] = [];
  columns.forEach((column: IColumn) => {
    keysFromColumns.push(column.key);
  });

  itemGroups.forEach((itemGroup: any) => {
    const rows = itemGroup.Items;
    rows.forEach((row: any) => {
      const attributesFromRow: any[] = row.Attributes;
      const formattedValuesFromRow: any[] = row.FormattedValues;
      const item: { [key: string]: any } = {};

      keysFromColumns.forEach((key: string) => {
        const formattedValue = formattedValuesFromRow.find(
          (value) => value.key === key
        );
        const attribute = attributesFromRow.find((value) => value.key === key);

        if (formattedValue !== undefined) {
          item[key] = formattedValue.value;
        } else if (attribute !== undefined) {
          item[key] = attribute.value.Value;
        } else if (key === 'EntityName') {
          item[key] = itemGroup.EntityName;
        } else {
          item[key] = '';
        }

        item['EntitySchemaName'] = itemGroup.EntitySchemaName;
        item['EntityId'] = row.Id;
        item['RawAttributes'] = row.Attributes;
      });

      items.push(item);
    });
  });

  return items;
};
