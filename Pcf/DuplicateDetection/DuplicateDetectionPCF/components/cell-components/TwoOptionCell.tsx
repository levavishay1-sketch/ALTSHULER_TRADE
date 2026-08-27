/* eslint-disable @typescript-eslint/no-explicit-any */
import * as React from 'react';
import {
  FontIcon,
  IColumn,
  Stack,
  mergeStyleSets,
  mergeStyles,
} from '@fluentui/react';

const iconClass = mergeStyles({
  fontSize: 18,
});

const iconClassNames = mergeStyleSets({
  indianRed: [{ color: 'indianred' }, iconClass],
  green: [{ color: 'green' }, iconClass],
});

export const TwoOptionCell = (
  item: any,
  column: IColumn,
  handleTabToDisplay: (tabName: string) => void
): React.JSX.Element => {
  if (item.raw.getValue(column.fieldName) === '1') {
    return (
      <Stack horizontal>
        <FontIcon
          aria-label='CompletedSolid'
          iconName='CompletedSolid'
          className={iconClassNames.green}
          onClick={() =>
            handleTabToDisplay(column.fieldName!)
          }
        />
      </Stack>
    );
  } else {
    return (
      <Stack horizontal>
        <FontIcon
          aria-label='StatusErrorFull'
          iconName='StatusErrorFull'
          className={iconClassNames.indianRed}
        />
      </Stack>
    );
  }
};
