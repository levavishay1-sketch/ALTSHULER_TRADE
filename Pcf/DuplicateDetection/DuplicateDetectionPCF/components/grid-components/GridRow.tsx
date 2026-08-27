/* eslint-disable @typescript-eslint/no-explicit-any */
/* eslint-disable react/display-name */
import * as React from 'react';
import { DetailsRow, IDetailsRowProps, IRenderFunction } from '@fluentui/react';
import { GridCell } from './GridCell';
import { Context } from '../../types/PcfCoreTypes';

const rowStyles = {
  root: {
    borderBottom: '0.5px solid lightgray',
  },
};

export const GridRow = (
  context: Context
): ((
  props: IDetailsRowProps | undefined,
  defaultRender?: IRenderFunction<IDetailsRowProps>
) => React.JSX.Element) => {
  return (
    props: IDetailsRowProps | undefined,
    defaultRender?: IRenderFunction<IDetailsRowProps>
  ) =>
    props && defaultRender ? (
      <DetailsRow
        {...props}
        onRenderItemColumn={GridCell(context)}
        styles={rowStyles}
      ></DetailsRow>
    ) : (
      <></>
    );
};
